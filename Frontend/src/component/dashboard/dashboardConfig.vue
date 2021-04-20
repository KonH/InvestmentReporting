<template>
	<div>
		<h3>Config</h3>
		<div class="form-group">
			<label>
				Name:
				<input v-model="dashboard.name" type="text" class="form-control" />
			</label>
			<div class="form-group">
				<button class="btn btn-primary" @click="save">Save</button>
			</div>
			<div class="form-group">
				<button class="btn btn-danger" @click="remove">Remove</button>
			</div>
		</div>
		<div class="form-group">
			<label>
				<b>Tags:</b>
			</label>
			<div v-for="tag in dashboard.tags" :key="tag">
				<dashboard-tag-card :dashboard="dashboard" :tag="tag" class="mb-2" @remove="onRemoveTag(tag.tag)" />
			</div>
			<div>
				<add-dashboard-tag-card v-if="canAddTag" :dashboard="dashboard" :tags="availableTags" @add="onAddTag" />
			</div>
		</div>
	</div>
</template>
<script lang="ts">
import { Options, Vue } from 'vue-class-component';
import { Prop } from 'vue-property-decorator';
import { AssetTagStateDto, DashboardConfigDto, DashboardConfigTagDto } from '@/api/meta';
import Backend from '@/service/backend';
import DashboardTagCard from '@/component/dashboard/dashboardTagCard.vue';
import AddDashboardTagCard from '@/component/dashboard/addDashboardTagCard.vue';
import { State } from 'vuex-class';

@Options({
	name: 'DashboardConfig',
	emits: ['save', 'remove'],
	components: {
		DashboardTagCard,
		AddDashboardTagCard,
	},
})
export default class DashboardConfig extends Vue {
	@State('tagState')
	tagState!: AssetTagStateDto;

	@Prop()
	dashboard!: DashboardConfigDto;

	async save() {
		await Backend.meta().dashboardConfig.dashboardConfigCreate(this.dashboard);
		this.$emit('save');
	}

	async remove() {
		await Backend.meta().dashboardConfig.dashboardConfigDelete({
			dashboard: this.dashboard.id ?? '',
		});
		this.$emit('remove');
	}

	async onRemoveTag(tag: string) {
		this.dashboard.tags = this.dashboard.tags?.filter((t) => t.tag != tag) ?? [];
		await this.save();
	}

	async onAddTag(tag: DashboardConfigTagDto) {
		this.dashboard.tags?.push(tag);
		await this.save();
	}

	get availableTags() {
		const tags = this.dashboard.tags?.map((t) => t.tag);
		const allTags = this.tagState.tags;
		if (tags && allTags) {
			return allTags.filter((t) => !tags.includes(t));
		}
		return [];
	}

	get canAddTag() {
		return this.availableTags.length > 0;
	}
}
</script>
