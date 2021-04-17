<template>
	<div>
		<h3>Config</h3>
		<div class="form-group">
			<label>
				Name:
				<input v-model="dashboard.name" type="text" class="form-control" />
			</label>
		</div>
		<div class="form-group">
			<label>
				<b>Tags:</b>
			</label>
			<div v-for="tag in dashboard.tags" :key="tag">
				<dashboard-tag-card :dashboard="dashboard" :tag="tag" class="mb-2" @remove="onRemove(tag.tag)" />
			</div>
			<div>
				<add-dashboard-tag-card :dashboard="dashboard" @add="onAdd" />
			</div>
		</div>
		<div class="form-group">
			<button class="btn btn-primary" @click="save">Save</button>
		</div>
	</div>
</template>
<script lang="ts">
import { Options, Vue } from 'vue-class-component';
import { Prop } from 'vue-property-decorator';
import { DashboardConfigDto, DashboardConfigTagDto } from '@/api/meta';
import Backend from '@/service/backend';
import DashboardTagCard from '@/component/dashboardTagCard.vue';
import AddDashboardTagCard from '@/component/addDashboardTagCard.vue';

@Options({
	name: 'DashboardConfig',
	emits: ['save'],
	components: {
		DashboardTagCard,
		AddDashboardTagCard,
	},
})
export default class DashboardConfig extends Vue {
	@Prop()
	dashboard!: DashboardConfigDto;

	async save() {
		await Backend.meta().dashboardConfig.dashboardConfigCreate(this.dashboard);
		this.$emit('save');
	}

	async onRemove(tag: string) {
		this.dashboard.tags = this.dashboard.tags!.filter((t) => t.tag != tag);
		await this.save();
	}

	async onAdd(tag: DashboardConfigTagDto) {
		this.dashboard.tags?.push(tag);
		await this.save();
	}
}
</script>
