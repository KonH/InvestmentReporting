<template>
	<span>
		<button v-if="!isExpanded" class="btn btn-sm btn-outline-primary" @click="expand">+</button>
		<span v-if="isExpanded" class="form-group">
			<label class="mr-2">
				Tag:
				<select ref="tag" class="form-control">
					<option v-for="tag in tags" :key="tag" :value="tag">{{ tag }}</option>
				</select>
			</label>
			<label class="mr-2">
				Target:
				<input ref="target" type="number" class="form-control" value="0" />
			</label>
		</span>
		<button v-if="isExpanded" class="btn btn-sm btn-primary mr-1" @click="add">+</button>
		<button v-if="isExpanded" class="btn btn-sm btn-outline-primary" @click="close">-</button>
	</span>
</template>
<script lang="ts">
import { Options, Vue } from 'vue-class-component';
import { Ref } from 'vue-property-decorator';
import { AssetTagStateDto, DashboardConfigTagDto } from '@/api/meta';
import { State } from 'vuex-class';

@Options({
	name: 'AddDashboardTagCard',
	emits: ['add'],
})
export default class AddDashboardTagCard extends Vue {
	@State('tagState')
	tagState!: AssetTagStateDto;

	@Ref('tag')
	tagInput!: HTMLInputElement;

	@Ref('target')
	targetInput!: HTMLInputElement;

	isExpanded = false;

	get tags() {
		return this.tagState.tags;
	}

	expand() {
		this.isExpanded = true;
	}

	async add() {
		const tag: DashboardConfigTagDto = {
			tag: this.tagInput.value,
			target: Number.parseFloat(this.targetInput.value),
		};
		this.$emit('add', tag);
		this.close();
	}

	close() {
		this.isExpanded = false;
		this.tagInput.value = '';
	}
}
</script>
