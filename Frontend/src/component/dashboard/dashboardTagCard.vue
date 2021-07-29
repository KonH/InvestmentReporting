<template>
	<span>
		<span>{{ tag.tag }} = {{ tag.target }}%</span>
		<button class="ml-1 btn btn-sm btn-outline-danger" @click="$emit('remove')">-</button>
		<button class="ml-1 btn btn-sm btn-outline-primary" @click="$emit('edit')">&#9998;</button>
	</span>
</template>
<script lang="ts">
import { Options, Vue } from 'vue-class-component';
import { Prop } from 'vue-property-decorator';
import { DashboardConfigTagDto } from '@/api/meta';

@Options({
	name: 'DashboardTagCard',
	emits: ['remove', 'edit'],
})
export default class DashboardTagCard extends Vue {
	@Prop()
	tag!: DashboardConfigTagDto;
}
</script>
